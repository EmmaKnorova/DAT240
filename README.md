# Start the project

To run the project execute: `docker compose up`.

## Acces the Page as a user

Go to `http://localhost:8080/`. Make sure to use `localhost` and not `127.0.0.1` when opening the website. The third party login from Microsoft does not support the IP equivalent of `localhost`.

## Access the Admin Overview

To access the admin interface, please visint `http://localhost:8080/Admin/Identity/Login`.

The default credentials for the admin user are: `admin@gmail.com:Admin123456789!`.
You will be prompted to change the password after the first login.

## Testing the External Payment System

For testing Stripe's payment system, use the following test parameters:

| Name    | Value |
| -------- | ------- |
| Card number  | 4242 4242 4242 4242 |
| Expiry | any future date (e.g. 12/30)  |
| CVC    | any 3 digits    |

For cancelled or declined payments, use test card: 4000 0000 0000 0002.

## Testing Thirdy Party Logins (Microsoft & Google)

Make sure to access the website on `localhost` - otherwise Microsoft's login won't work. Microsoft only supports `HTTP` over `localhost`.
The best way to test the login is by using a private Microsoft account.
Microsoft accounts from an organization have to be approved by organization's admin user.

## For Developers

### Run Tests

Go to `webapp.Tests` and type:

```bash
dotnet test
```

### Docker

To rebuild after every change during development

```bash
docker compose up --watch
```

### Commit Structure

Examples:

* [Update] [README.md] Changed the project description.
* [Add] [User.cs] Created entity.
* ...

### Branch Naming Convention

To keep the repository organized, follow this naming pattern for all branches:

```text
<type>/<short-description>
```

Where `<type>` can be one of the following:

* feature/ → For new features or functionality
* bugfix/ → For fixing bugs in existing code
* refactor/ → For improving code structure without changing behavior
* test/ → For adding or modifying tests
* docs/ → For updating documentation only
