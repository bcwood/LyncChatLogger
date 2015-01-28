# Lync Chat Logger
Log Lync chats to simple text files, similar to Pidgin.

I don't like Lync's default behavior of logging conversations in Outlook. Logging them to a simple text file makes them more searchable, and more portable.

Lync doesn't really have a plugin model, so you will have to run this application separately from Lync. It will either connect to a running instance of Lync, or start a new one. In other words, you can just start this application, and it will automatically start Lync for you. It also tracks when Lync shuts down and will shut itself down at the same time.
