# Instructions to Set Up the Project

## Step 1: Create the Database
1. Navigate to the `db` folder.
2. Execute the `CreateDatabaseScript.sql` script on your SQL Server to create the database and necessary tables.

## Step 2: Update the Connection String
1. Open the file `src/Payphone_Assestment/Payphone_Assestment.Api/appsettings.json`.
2. Update the `DefaultConnection` value in the `ConnectionStrings` section with your SQL Server credentials. Example:
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=dbPayPhoneChallenge;User Id=sa;Password=YourPassword;Encrypt=True;TrustServerCertificate=True;"
   }
   ```

## Step 3: Run the API
1. Open the project in your IDE (e.g., JetBrains Rider).
2. Ensure that the `Payphone_Assestment.Api` project is set as the startup project.
3. Run the application. This will start the server on the configured port (default: `https://localhost:5001`).

---

## Testing Endpoints via Swagger

1. Once the API is running, access Swagger at the URL: `https://localhost:5001/swagger`.
2. **Endpoints for `UserController`:**
    - **POST /api/user/register**: Registers a new user. Provide a JSON body with `Username`, `Password`, and other required data.
    - **POST /api/user/login**: Logs in with a registered user. Provide a JSON body with `Username` and `Password`. You will receive a JWT token in the response.
3. **Endpoints for `WalletController` (Token required):**
    - **POST /api/wallet/create**: Creates a new wallet. Provide a JSON body with `Name` and `InitialBalance`.
    - **GET /api/wallet/list**: Retrieves all wallets for the authenticated user.
    - **GET /api/wallet/{id}**: Retrieves a specific wallet by its ID.
    - **PUT /api/wallet/{id}**: Updates an existing wallet. Provide a JSON body with the data to modify.
    - **DELETE /api/wallet/{id}**: Deletes a specific wallet by its ID.
    - **POST /api/wallet/transfer**: Performs a transfer between wallets. Provide a JSON body with the transfer details.

**Note:** To test protected endpoints, copy the JWT token obtained during login and add it using the `Authorize` button in Swagger.