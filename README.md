# d2WhisperMonitor
Takes a screenshot of the specified part of your monitor and checks for the whisper text color(bright green). If it's found you will be emailed with a screenshot.

Steps to use would be:
* set up a gmail account(preferrably a separate one from your main) to work as the sender
** You'll need to enable less secure as per apps: https://help.dreamhost.com/hc/en-us/articles/115001719551-Troubleshooting-GMAIL-SMTP-authentication-errors
* Edit the app.config per below
* Build the application
* run it(there's no UI).
* Open Project diablo 2 to the chat window. You should now get an email for a whisper.
* If you want to stop the application just use task manager.

App.config settings:
* senderAddress: the email address you send emails from. has to be a gmail
* senderPassword: the email addresses password
* recipientAddress: where the emails will be sent to
* rectangleParams: x,y,width,height of the screenshot to take to monitor for whispers
