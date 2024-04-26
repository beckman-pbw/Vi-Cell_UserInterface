# Trouble Shooting {#trouble_shooting}

## HawkeyeCore.dll failed to load

You need to rebuild everything in ApplicationSource (C++ code). Steps:
1. Save/commit all your work because this will reset everything locally
2. Close all Visual Studio windows with anything Scout-related
3. Open HawkeyeApplicationSource in git bash
4. Run the following commands:
	git clean -xffd
	git reset --hard head
	git pull
	mvn clean
	mvn package
5. After a successful "mvn package", open HawkeyeApplicationSource\Hawkeye.sln in VS2019
6. Rebuild DBif project
7. Rebuild DataAccess project
8. Rebuild HawkeyeCore project

## Silent Admin login failure

You need to rebuild your database. Steps:
1. Start PgAdmin
    Password is usually "postgres"
    Local DB password is usually "$3rgt$0P"
2. Drop the ViCellDB_template database (in PostgreSQL 10)
3. Drop the ViCellDB database (in PostgreSQL 10)
4. Open HawkeyeApplicationSource\Installer\SchemaInstaller.bat in text editor
5. Find/Replace: %username% / %PGUSER%
6. Save the file
7. Run HawkeyeApplicationSource\Installer\SchemaInstaller.bat in cmd window