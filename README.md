# Instrucciones para Levantar el Proyecto

## Paso 1: Creación de la Base de Datos
1. Navega a la carpeta `db`.
2. Ejecuta el script `CreateDatabaseScript.sql` en tu servidor SQL Server para crear la base de datos y las tablas necesarias.

## Paso 2: Actualizar el Connection String
1. Abre el archivo `src/Payphone_Assestment/Payphone_Assestment.Api/appsettings.json`.
2. Actualiza el valor de `DefaultConnection` en la sección `ConnectionStrings` con las credenciales de tu servidor SQL Server. Ejemplo:
   ```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=dbPayPhoneChallenge;User Id=sa;Password=TuContraseña;Encrypt=True;TrustServerCertificate=True;"
   }
   ```

## Paso 3: Ejecución de la API
1. Abre el proyecto en tu IDE (por ejemplo, JetBrains Rider).
2. Asegúrate de que el proyecto `Payphone_Assestment.Api` esté configurado como el proyecto de inicio.
3. Ejecuta la aplicación. Esto levantará el servidor en el puerto configurado (por defecto, `https://localhost:5001`).

---

## Pruebas de los Endpoints desde Swagger

1. Una vez que la API esté en ejecución, accede a Swagger en la URL: `https://localhost:5001/swagger`.
2. **Endpoints de `UserController`:**
    - **POST /api/user/register**: Registra un nuevo usuario. Proporciona un cuerpo JSON con `Username`, `Password` y otros datos requeridos.
    - **POST /api/user/login**: Inicia sesión con un usuario registrado. Proporciona un cuerpo JSON con `Username` y `Password`. Obtendrás un token JWT en la respuesta.
3. **Endpoints de `WalletController` (Token requerido):**
    - **POST /api/wallet/create**: Crea una nueva billetera. Proporciona un cuerpo JSON con `Name` y `InitialBalance`.
    - **GET /api/wallet/list**: Obtiene todas las billeteras del usuario autenticado.
    - **GET /api/wallet/{id}**: Obtiene una billetera específica por su ID.
    - **PUT /api/wallet/{id}**: Actualiza una billetera existente. Proporciona un cuerpo JSON con los datos a modificar.
    - **DELETE /api/wallet/{id}**: Elimina una billetera específica por su ID.
    - **POST /api/wallet/transfer**: Realiza una transferencia entre billeteras. Proporciona un cuerpo JSON con los datos de la transferencia.

**Nota:** Para probar los endpoints protegidos, copia el token JWT obtenido en el login y agrégalo en el botón `Authorize` de Swagger.