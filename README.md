# Password Security — PAP

Desktop password manager built as my final course project (PAP), using C#, .NET 8, WinUI 3 (Windows App SDK) and MongoDB.

Source code: [`Códigos/Projeto_PAP`](Códigos/Projeto_PAP)

## Configuration

The app reads its settings from environment variables, so no credentials are stored in the code.

| Variable | Purpose |
|---|---|
| `MONGODB_CONNECTION_STRING` | MongoDB connection string |
| `PAP_SENDER_EMAIL` | Gmail address that sends the verification and security emails |

### Email sending (Gmail API)

The app sends emails (verification codes, security alerts) through the Gmail API with OAuth 2.0.
The Gmail account originally used by the project is no longer active, so to enable email sending:

1. Create a project in [Google Cloud Console](https://console.cloud.google.com/), enable the **Gmail API** and create an **OAuth client ID** of type *Desktop app*.
2. Download the client file, rename it to `credentials.json` and place it next to the app's executable.
3. Set `PAP_SENDER_EMAIL` to the Gmail address of the account you will authorize.
4. Run the app: the first email opens the browser to authorize that account; the token is then saved locally in the `token` folder.

`credentials.json` and the `token` folder contain secrets and must not be committed.
