# Password Security — PAP

Desktop password manager built as my final course project (PAP), using C#, .NET 8, WinUI 3 (Windows App SDK) and MongoDB.

Source code: [`Códigos/Projeto_PAP`](Códigos/Projeto_PAP)

## Configuration

The app reads its settings from environment variables, so no credentials are stored in the code.

| Variable | Purpose |
|---|---|
| `MONGODB_CONNECTION_STRING` | MongoDB connection string |
| `PAP_SENDER_EMAIL` | Gmail address that sends the verification and security emails |

### Database (MongoDB Atlas)

The app stores its accounts in a MongoDB database called `PAP`, in a collection called `Users`.
Create them exactly with these names (step 4): the names are case-sensitive, so `pap` or `users` will not work.

1. **Create a free cluster**
   - Sign in to [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) (create a free account if you don't have one).
   - Click **Create**, select the **M0** (free) tier, choose a provider and a region, give the cluster a name and click **Create**.
2. **Create a database user** (the *Security Quickstart* opens after the cluster is created)
   - Enter a **Username** and a **Password** (or use the password Atlas suggests) and click **Create Database User**.
   - Save the password somewhere safe: Atlas will not show it again.
3. **Allow your IP address**
   - In the same screen, click **Add My Current IP Address**, then **Finish and Close**.
   - If your IP changes later, add the new one in **Network Access**.
4. **Create the database and the collection**
   - In the sidebar, click **Data Explorer**.
   - In the **Connections** list, hover over your cluster and click the **+** icon to open **Create Database**.
   - **Database Name:** `PAP`
   - **Collection Name:** `Users`
   - Click **Create Database**. `PAP` should now appear with the `Users` collection inside it.
5. **Copy the connection string**
   - On the **Clusters** page, click **Connect** on your cluster, choose **Drivers** and select **C#/.NET**.
   - Copy the connection string. It looks like this:
     ```
     mongodb+srv://<db_username>:<db_password>@<cluster-name>.xxxxx.mongodb.net/?retryWrites=true&w=majority
     ```
   - Replace `<db_password>` (and `<db_username>`, if it appears) with the user you created in step 2.
     If the password has special characters such as `@`, `:` or `/`, they must be URL-encoded (for example, `@` becomes `%40`).
6. **Set the environment variable** (see the next section).

### Setting the environment variables (Windows)

Open **PowerShell** (or the Command Prompt) and run, replacing the values in quotes with your own:

```powershell
setx MONGODB_CONNECTION_STRING "mongodb+srv://user:password@cluster-name.xxxxx.mongodb.net/?retryWrites=true&w=majority"
setx PAP_SENDER_EMAIL "your.address@gmail.com"
```

`setx` saves the variables for your Windows user. They are only visible to programs opened **after** the command, so close and reopen Visual Studio (or the terminal) before running the app.

To check the value, open a **new** PowerShell window and run:

```powershell
[Environment]::GetEnvironmentVariable("MONGODB_CONNECTION_STRING", "User")
```

You can also set them without the terminal: **Start** → search for *environment variables* → **Edit environment variables for your account** → **New…**.

If `MONGODB_CONNECTION_STRING` is missing, the app stops with the error *"MONGODB_CONNECTION_STRING environment variable is not set!"*.

> Never put the real connection string in the code or in a commit: it contains the database password.

### Email sending (Gmail API)

The app sends emails (verification codes, security alerts) through the Gmail API with OAuth 2.0.
The Gmail account originally used by the project is no longer active, so to enable email sending:

1. Create a project in [Google Cloud Console](https://console.cloud.google.com/), enable the **Gmail API** and create an **OAuth client ID** of type *Desktop app*.
2. Download the client file, rename it to `credentials.json` and place it next to the app's executable.
3. Set `PAP_SENDER_EMAIL` to the Gmail address of the account you will authorize.
4. Run the app: the first email opens the browser to authorize that account; the token is then saved locally in the `token` folder.

`credentials.json` and the `token` folder contain secrets and must not be committed.
