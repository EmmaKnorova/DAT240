# Start the project

## Access the Admin Overview

To access the admin interface, please visint `http://127.0.0.1:8080/Admin/Identity/Login`.
The default credentials for the admin user are: `admin@gmail.com:Admin123456789!`.
You will be prompted to change the password after the first login.

## For Developers

### Docker

To build the project:

```bash
docker compose up --build
```

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
