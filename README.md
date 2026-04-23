# 🏥 SaludTotal API

## 📌 Descripción

API REST desarrollada en .NET para la gestión de una clínica. Permite administrar pacientes, doctores, citas médicas y expedientes clínicos, aplicando buenas prácticas de arquitectura, seguridad y manejo de datos.

---

## 🚀 Tecnologías utilizadas

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* ASP.NET Identity
* JWT Authentication
* SQL Server
* Docker
* Mapster

---

## ⚙️ Configuración del proyecto

1. Clonar repositorio

```bash
git clone <url-del-repositorio>
cd SaludTotalAPI
```

2. Configurar variables de entorno

## 🔑 Variables de entorno

Este proyecto utiliza variables de entorno para manejar información sensible.

### Variables requeridas

```bash
Jwt__Key= tu_clave_super_secreta
ConnectionStrings__DefaultConnection= Server=localhost,1434;Database=SaludTotalDB;User Id=sa;Password=tu_password;
MSSQL_SA_PASSWORD= TU_PASSWORD
```

### 📌 Explicación

* `Jwt__Key` → clave para firmar los tokens JWT
* `ConnectionStrings__DefaultConnection` → cadena de conexión a SQL Server
* `MSSQL_SA_PASSWORD` → contraseña del contenedor SQL Server

---

3. Ejecutar base de datos con Docker

```bash
## 🐳 Base de datos con Docker

El proyecto utiliza una imagen de SQL Server para crear un contenedor en Docker para facilitar la ejecución.

### docker-compose.yml

```yaml
services:
  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver2022_saludtotal
    ports:
      - "1434:1433"
    environment:
      ACCEPT_EULA: "1"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
      MSSQL_PID: "Developer"
    volumes:
      - sqlserverdata_saludtotal:/var/opt/mssql

volumes:
  sqlserverdata_saludtotal:
```

---

## 🛠️ Ejecución completa

1. Levantar base de datos

```bash
docker-compose up -d
```

2. Ejecutar migraciones

```bash
dotnet ef database update
```

3. Ejecutar API

```bash
dotnet run
```

---

## 🌱 Data Seeder

Al iniciar la aplicación se ejecuta automáticamente un seeder que crea:

* Roles:

  * Admin
  * Doctor
  * Patient

* Usuarios de prueba:

  * Admin
  * Doctor
  * Patient

* Especialidades médicas

* Pacientes y doctores asociados a un usuario

* Citas médicas

* Expedientes clínicos

🔐 Las contraseñas se generan automáticamente y se muestran en consola.

---

## 🔐 Seguridad implementada

* Autenticación mediante JWT
* Autorización basada en roles
* Validación de acceso:

  * Pacientes y doctores solo acceden a su información
  * Administrador tiene acceso total

* Rate Limiting:

  Se limita la cantidad de solicitudes por cliente para evitar abuso de la API.
  Ejemplo: máximo 5 solicitudes cada 60 segundos.

  En caso de exceder el límite, la API responde con:
  HTTP 429 - Too Many Requests

* Validaciones mediante DTOs

---

## 📊 Endpoints principales

### 🔑 Autenticación

* POST `/api/v1/auth/registerPatient`
* POST `/api/v1/auth/login`
* POST `/api/v1/auth/changePassword`

---

### 👨‍⚕️ Doctores

* GET `/api/v1/doctors`
* GET `/api/v1/doctors/{id}`
* POST `/api/v1/doctors`

---

### 🧑‍🤝‍🧑 Pacientes

* GET `/api/v1/patients`
* GET `/api/v1/patients/me`
* GET `/api/v1/patients/{id}`
* GET `/api/v1/patients/byRut/{rut}`

---

### 📅 Citas

* GET `/api/v1/appointments/{id}`
* GET `/api/v1/appointments/doctor/{doctorId}`
* POST `/api/v1/appointments`
* PATCH `/api/v1/appointments/{appointmentId}/status`

---

### 📄 Expediente Médico

* GET `/api/v1/medicalrecords/me`
* GET `/api/v1/medicalrecords/patient/{patientId}`
* PUT `/api/v1/medicalrecords/patient/{patientId}`

---

## 🧱 Arquitectura

El proyecto sigue una arquitectura en capas:

* Controllers → Manejo de endpoints
* DTOs → Validación y transferencia de datos
* Repository → Acceso a datos
* Models → Entidades del sistema
* Data → DbContext y Seeder

---

## 🧪 Buenas prácticas implementadas

* Separación de responsabilidades
* Uso de DTOs
* Control de errores
* Validación de acceso por usuario
* Código limpio y mantenible
* Uso de variables de entorno
* Uso de Docker para entorno reproducible

---

## 📌 Notas finales

Este proyecto fue desarrollado como prueba técnica, enfocándose en:

* Seguridad
* Buenas prácticas backend
* Diseño de APIs REST
* Manejo correcto de autenticación y autorización

---
