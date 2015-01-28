using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Microsoft.Lync.Model.Conversation;

namespace LyncChatLogger
{
	public static class ChatLog
	{
		private static Dictionary<string, string> _logs = new Dictionary<string, string>();

		public static void Open(Conversation conversation)
		{
			Participant participant = LyncHelper.GetConversationParticipant(conversation);

			if (participant != null)
			{
				string username = LyncHelper.GetContactUsername(participant.Contact);

				DebugLog.Write("Conversation with {0} started at {1} {2}", username, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
			}
		}

		public static void Close(Conversation conversation)
		{
			string conversationId = (string) conversation.Properties[ConversationProperty.Id];
			if (_logs.ContainsKey(conversationId))
				_logs.Remove(conversationId);

			Participant participant = LyncHelper.GetConversationParticipant(conversation);

			if (participant != null)
			{
				string username = LyncHelper.GetContactUsername(participant.Contact);

				DebugLog.Write("Conversation with {0} ended at {1} {2}", username, DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
			}
		}

		public static void Write(InstantMessageModality messageInfo, string text)
		{
			string username = LyncHelper.GetContactUsername(messageInfo.Participant.Contact);
			string path = GetLogFilePath(messageInfo);

			using (var writer = new StreamWriter(path, true))
			{
				writer.Write("[{0} {1}] {2}: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), username, text);
			}
		}

		private static string GetLogFilePath(InstantMessageModality messageInfo)
		{
			string path = ConfigurationManager.AppSettings["LOG_PATH"];

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			Participant participant = LyncHelper.GetConversationParticipant(messageInfo.Conversation);
			string username = LyncHelper.GetContactUsername(participant.Contact);

			path += "/" + username;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			string conversationId = (string) messageInfo.Conversation.Properties[ConversationProperty.Id];
			string filename = GetConversationFilename(conversationId);

			path += "/" + filename;

			return path;
		}

		private static string GetConversationFilename(string conversationId)
		{
			string filename;

			if (!_logs.ContainsKey(conversationId))
			{
				filename = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
				_logs.Add(conversationId, filename);
			}
			else
			{
				filename = _logs[conversationId];
			}

			return filename;
		}
	}
}
