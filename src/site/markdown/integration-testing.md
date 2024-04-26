Integration Testing
-------------------

In order to run/debug the UserInterface software on a worksation with visual studio, a few steps must be done to prepare the system for testing.  The steps below will prepare a workstation to debug the UserInterface application:

1.  Install Git for windows: https://git-scm.com/downloads

2.  Install VisualStudio 2015
    * Make sure and also select the "Visual C++ -> Microsoft Foundation Classes for C++" feature under the Features list as well.
    ** If you want to user a newer version of VisualStudio, you can but you will need to edit the pom.xml file to user your version of MSBUILD.exe.

3.  Verify that .Net 4.7.2 is installed

4.  Install JRE
    * Download JRE: http://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html
    * Open Source JRE (OpenJDK 8) here: https://adoptopenjdk.net/
 
5.  Set JAVA_HOME (if the JRE package didn't set path during installation)
    * Hit ctrl-esc
    * Type: "environment variables"
    * Select tool to edit system environment variables
    * Select "environment variables"
    * Add `JAVA_HOME` as a system environment variable with the value: `C:\Program Files\Java\VERSION_INSTALLED_ABOVE\`

6. Install Maven 
    * Download: https://archive.apache.org/dist/maven/maven-3/3.3.9/binaries/apache-maven-3.3.9-bin.zip
    * Unzip to `C:\Dev` and add the directory `C:/Dev/apache-maven-3.3.9/bin` folder to your `PATH` environment variable

7. Fork the UserInterface repository using the GitHub web app: http://svusftcgit/Hawkeye/UserInterface

8. Clone your new fork:
    * Open a command prompt:
        * Navigate to the location you want your source code in
        * Run `git clone http://10.101.20.145/YOUR_USERNAME/UserInterface`

9. Update dependency for HawkeyeCore
    * Manual:
        * Edit pom.xml
        * Find the "dependency" for "backend"
        * Change the version to match the version you want to test with
    * Automatic:
         ```
         mvn versions:use-latest-releases -Dincludes="com.beckman.particle.hawkeye"
         ```
10. If using a newer version of Visual Studio:
    * Edit pom.xml and pdate the MsBuild.exe path to use yours.

11. Disable hardware interaction:
    * Edit environment.config:
        * Edit [CodeCheckout]\UserInterface\ScoutUtilities\UIConfiguration\environment.config
    * Ensure that the line containing `IsFromHardware` has it set to false:
         `isFromHardware="False"`

12. Run Maven integration test WARNING: This will destroy the C:\Instrument completely!!  Back it up first if you need it.
    * `mvn integration-test`
    * [This should now download a whopping boatload of stuff and then begin a compilation of the source code]
    * This will also populate a `c:\Instrument` directory with everything it needs to run

13. Debug (build & run)
    * Open Visual Studio
    * Load the solution from `[CodeCheckout]\UserInterface\ScoutUI.sln`
    * Select the 'Debug'/'AnyCPC' options
    * In the ApiProxies project update the reference for OpenCVWrapper to point to the Release version
        * Path: `<clone-path>\target\Release\OpenCVWrapper.dll`
        * `<clone-path>` is the path to your clone, e.g. `C:\DEV\Hawkeye\UserInterface`
    * Set `ScoutUI` as the Startup Project (right-click it in the solution explorer)
    * Build/Run the solution
