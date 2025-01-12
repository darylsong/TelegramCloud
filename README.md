# TelegramCloud

.NET console application for using Telegram as a cloud storage.

## Description

Files uploaded using this application will be saved to a private Telegram chat between the user and a Telegram bot created by the user.

As Telegram imposes a 20MB limit on files shared using the Telegram bot API, files larger than 20MB will be chunked before they are uploaded.

## Usage

### Installing the .NET SDK

To build the application binaries, you will need to have the .NET SDK (at least version 8.0) installed. Please refer to [this page](https://dotnet.microsoft.com/en-us/download) for details on how to install the .NET SDK.

Once you have the .NET SDK installed, you can build the application by running the following command in the project's root directory:

```dotnet build --output build --configuration Release```

The compiled binaries should be generated in the `/build` directory.

### Setting up Telegram bot

Next, you will need to create a Telegram bot. Please refer to [this guide](https://core.telegram.org/bots/features#creating-a-new-bot) on how to create a new Telegram bot. You will need to configure the application to use your new Telegram bot by setting the bot's token using the following command:

```./TelegramCloud configuration set --token {TELEGRAM_BOT_TOKEN}```

You will then need to start a private chat with the Telegram bot. Please refer to [this guide](https://gist.github.com/nafiesl/4ad622f344cd1dc3bb1ecbe468ff9f8a) on how to retrieve the chat ID. You will need to configure the application to use your private chat with the Telegram bot by setting the chat ID using the following command:

```./TelegramCloud configuration set --chat-id {TELEGRAM_CHAT_ID}```

Once this is done, your application should be properly configured to use your private chat with the new Telegram bot to store your files.

### Uploading and downloading files

To upload a file, you can use the following command:

```./TelegramCloud file upload {PATH_TO_FILE}```

To view a list of all uploaded files, you can use the following command:

```./TelegramCloud file list```

To download a file, you can use the following command:

```./TelegramCloud file download {FILE_ID}```

You can use the following command to retrieve more information on the various commands available:

```./TelegramCloud help```