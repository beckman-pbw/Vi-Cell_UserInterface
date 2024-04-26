# Active Directory {#active_directory}

ScoutX supports both local security and using a customer's Active Directory
server. The ScoutX roles (Normal, Advanced, Admin) are mapped to existing
AD security groups.

## Development and Testing

Currently, there is not a VM with an Active Directory installation to
be used as a test environment. We are using the production Beckman
Active Directory server. To use the Beckman AD server, you will need
to install the BEC-Root-CA.cer.
In the Active Directory Configuration dialog,
specify the server and base DN as:
```
Server: svusawscdc02.global.bcecorp.net
Base DN: global.bcecorp.net 
```
NOTE: If this is your first time filling out the Active Directory config, the Base DN field will be pre-populated for you and 
may be slightly different than the base dn above. Either the pre-populated value or the base dn given above can be used, 
but the server MUST be svusawscdc02.global.bcecorp.net. Note that the default port is 636, which is the secure protocol. Port 389
is insecure, however all Beckman AD servers that supported the insecure protocol are being decommissioned.

In order to be use your Beckman credentials as an administrator, use the
following Role Mapping:
```
Normal: "BEC Loveland Site Users"
Advanced: "BEC Loveland - R and D"
Admin: "BEC Loveland - Software Engineers"
```
Note: If you are a contingent worker, you may not have the "BEC Loveland - Software Engineers" security group, so your Beckman User would not be an admin. You could instead use, "Fort Collins Site Users".

If you need to select a different role (non-admin) you can install the [Active Directory Explorer v1.44](https://docs.microsoft.com/en-us/sysinternals/downloads/adexplorer)
to browse the available security groups, and pick one you do not have. You could also use
the [AD Browser 6.10](https://www.ldapsoft.com/adbrowserfree.html), a Free Active Directory® Browser by LDAPSoft.
In order to determine what roles you do have, you can use the command from a command window:
```
gpresult /V
```
This will list all the security groups you belong to, as well as a bunch of other info.

## Design Notes
Currently, the backend performs no validation for either the server, baseDN or roles.
The frontend validates the baseDN and security groups when saving the config. If any of these fields are invalid,
the user is informed and the config is not saved. The AD Server field is not validated, and if the value is incorrect,
users other than factory_admin will be unable to log in.
It is not within scope to provide drop down list boxes for the available roles.
