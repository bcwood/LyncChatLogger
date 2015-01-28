using System;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace LyncChatLogger
{
	public partial class MainForm : Form
	{
		private System.Threading.Timer _lyncStatusTimer;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs args)
		{
			try
			{
				DebugLog.Write("STARTUP");

				LyncClient client = LyncHelper.GetLyncClient();

				if (client == null)
				{
					Application.Exit();
					return;
				}

				// subscribe to conversation added/removed events
				client.ConversationManager.ConversationAdded += ConversationAdded;
				client.ConversationManager.ConversationRemoved += ConversationRemoved;

				//// subscribe to contact availibility changes
				//foreach (var group in client.ContactManager.Groups)
				//{
				//	//Debug.WriteLine("{0}: {1}", group.Name, group.Count);

				//	foreach (var contact in group)
				//	{
				//		contact.ContactInformationChanged += ContactInformationChanged;

				//		ContactStatus status = GetContactStatus(contact);

				//		if (status != null)
				//		{
				//			Debug.WriteLine("{0} initial status: {1} {2}", status.Username, status.Availability, status.Activity);
				//		}
				//	}
				//}

				DebugLog.Write("Waiting for incoming messages...");

				// check for shutdown of lync client
				_lyncStatusTimer = new System.Threading.Timer(CheckLyncProcessStatus, null, 1000, 1000);
			}
			catch (Exception ex)
			{
				LogException("InitializeLogger", ex);
				LyncHelper.ShutdownLync();
				Environment.Exit(0);
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs args)
		{
			LyncHelper.ShutdownLync();
		}

		//private void ContactInformationChanged(object sender, ContactInformationChangedEventArgs args)
		//{
		//	if (args.ChangedContactInformation.Contains(ContactInformationType.Availability))
		//	{
		//		ContactStatus status = LyncHelper.GetContactStatus((Contact) sender);

		//		if (status != null)
		//		{
		//			Debug.WriteLine("{0} updated status: {1} {2}", status.Username, status.Availability, status.Activity);
		//		}
		//	}
		//}

		private void ConversationAdded(object sender, ConversationManagerEventArgs args)
		{
			try
			{
				ChatLog.Open(args.Conversation);

				// subscribe to event for new participants being added to this conversation
				args.Conversation.ParticipantAdded += ParticipantAdded;
			}
			catch (Exception ex)
			{
				LogException("ConversationAdded", ex);
				throw;
			}
		}

		private void ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs args)
		{
			try
			{
				var messageInfo = (InstantMessageModality) args.Participant.Modalities[ModalityTypes.InstantMessage];

				// subscribe to event for new messages from this participant
				messageInfo.InstantMessageReceived += MessageReceived;
			}
			catch (Exception ex)
			{
				LogException("ParticipantAdded", ex);
				throw;
			}
		}

		private void MessageReceived(object sender, MessageSentEventArgs args)
		{
			try
			{
				var messageInfo = (InstantMessageModality) sender;
				ChatLog.Write(messageInfo, args.Text);
			}
			catch (Exception ex)
			{
				LogException("MessageReceived", ex);
				throw;
			}
		}

		private void ConversationRemoved(object sender, ConversationManagerEventArgs args)
		{
			try
			{
				ChatLog.Close(args.Conversation);
			}
			catch (Exception ex)
			{
				LogException("ConversationRemoved", ex);
				throw;
			}
		}

		private void CheckLyncProcessStatus(object state)
		{
			if (LyncHelper.GetLyncProcess() == null)
			{
				DebugLog.Write("Lync is no longer running, exiting LyncChatLogger");
				DebugLog.Write("SHUTDOWN{0}", Environment.NewLine);
				Environment.Exit(0);
			}
		}

		private void LogException(string source, Exception ex)
		{
			DebugLog.Write("ERROR in {0}: {1}{2}{3}", source, ex.Message, Environment.NewLine, ex.StackTrace);
		}
	}
}
