using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace LyncChatLogger
{
	public static class LyncHelper
	{
		public static string GetContactUsername(Contact contact)
		{
			string username = contact.Uri;

			// remove sip: prefix
			if (username.StartsWith("sip:"))
				username = username.Substring(4);

			// remove @ and everything after
			if (username.IndexOf("@") > 0)
				username = username.Substring(0, username.IndexOf("@"));

			return username;
		}

		public static ContactStatus GetContactStatus(Contact contact)
		{
			try
			{
				return new ContactStatus
				{
					Username = GetContactUsername(contact),
					Availability = (ContactAvailability) contact.GetContactInformation(ContactInformationType.Availability),
					Activity = (string) contact.GetContactInformation(ContactInformationType.Activity)
				};
			}
			catch (NotSignedInException)
			{
				return null;
			}
		}

		public static Participant GetConversationParticipant(Conversation conversation)
		{
			// get other particpant in chat
			return (from Participant p in conversation.Participants
					where p != conversation.SelfParticipant
					select p).SingleOrDefault();
		}

		public static LyncClient GetLyncClient()
		{
			// start Lync client if not already running
			if (GetLyncProcess() == null)
				Process.Start("Lync");

			DebugLog.Write("Connecting to Lync client...");

			LyncClient client = null;
			bool isConnected = false;

			// wait for lync client to initialize and sign in
			while (!isConnected)
			{
				try
				{
					client = LyncClient.GetClient();
				}
				catch (ClientNotFoundException) { }

				if (client == null || client.State != ClientState.SignedIn)
				{
					Thread.Sleep(1000);
				}
				else
				{
					isConnected = true;
				}
			}

			return client;
		}

		public static Process GetLyncProcess()
		{
			return Process.GetProcessesByName("Lync").FirstOrDefault();
		}

		public static void ShutdownLync()
		{
			DebugLog.Write("LyncChatLogger closing, shutting down Lync");

			Process p = LyncHelper.GetLyncProcess();
			if (p != null)
			{
				p.Kill();
			}

			DebugLog.Write("SHUTDOWN{0}", Environment.NewLine);
		}
	}

	public class ContactStatus
	{
		public string Username { get; set; }
		public ContactAvailability Availability { get; set; }
		public string Activity { get; set; }
	}
}
