# RPTExporter

This tool allows you to customise the arguments fed into parameters. It does not interfere with the RPT file, restricting
itself to SAP's definition of 'viewing' the file, and cannot be used to adjust the design. This is necessary in order to
maintain the legality of the application (see below). It only inserts arguments into parameters in memory, and only sets
the connection information's SQL password, as this cannot be stored in the RPT file when saving from Crystal Reports
Designer.

It is currently built to expect an SQL login called 'reader' with the password 'reader', with permissions restricted to
reading the database. **If you intend to use this application on anything other than an air-gapped network or computer, and
especially if your database contains sensitive information, you *must* amend the source code to prompt the user for a
password on export**. The username for the database connection *must* be set in Crystal Reports Designer, as setting this in
RPT Exporter *may* violate SAP's terms of use by exceeding their definition of 'viewing' the file (their terms are quite
vague in this regard).

### Important Notes on Licensing

The Crystal Reports designer requires a separate licence for each installed copy. The Crystal Reports runtime, however, is
licence-free, so long as it is not used to edit or otherwise manipulate the RPT files it works with. RPT Exporter has been
designed to work with RPT files without adjusting the file data in any way in order to preserve the legality of having it
installed on multiple machines.

Note that the MIT licence only applies to the code in this repository, and does not extend to any software developed by SAP.

## Build

Simply clone the repository - it should build successfully right off the bat. In order to export files from the application,
you will need to download Crystal reports' .NET runtime from their website.
