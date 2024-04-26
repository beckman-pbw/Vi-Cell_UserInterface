Build Requirements & Best Practices
-----------------------------

To produce software artifacts that are reproduceable (long term) we leverage an automated build system.  This build system is designed based on industry best practices which include:
  1. Guarantee long-term reproducibility 
  2. Guarantee long-term archival of artifacts
  3. Documented release notes describing each release
  4. Automatic versioning of artifacts
  5. Automatic tagging the software repository with each release
  6. Separation of software source and binary artifacts

With these goals in mind, we have a few requirements that must be met to ensure that our build chain is consistent and straight forward to manage.

Maven Build System
------------------

Our build system, at its core, leverages the Apache Maven build system https://maven.apache.org/.  We use maven to:
* Manage dependencies
* Compile software
* Inject version into artifacts
* Tag software during release
* Archive software to Amazon S3 after build is complete
* Automatic generation of Release notes that are archived with the released artifacts

Details of each of these are outlined below:

Dependencies
------------

A maven project defines its dependencies in the project's `pom.xml`.  During build time these dependencies are pulled from Beckman's long term artifact repository onto the build machine (unless they are cached in the local computer's maven cache) so that they can be used for the build.  This requirement means that that projects that use static libraries that are  checked in with the source code, or those using another artifact management system such as NuGet must change.  

    Requirement: All dependent libraries or external dependencies that are used at build time must be maven artifacts that are archived in the Beckman long term artifact repository.  There  is a video that demonstrates how to do this: https://s3-us-west-2.amazonaws.com/shared-external/videos/BuildVideos.html

Compile Software
----------------
Because our builds must be done automatically, without human intervention, software builds must be able to be done "headless" meaning, without a human driving.  Microsoft supports a tool called `msbuild.exe` that is very useful for building Microsoft-based projects.  KDS, CodeWarrior and other linux-based build systems also have ways to be build offline.

    Requirement: Build steps must be able to be done offline.

Versioning Artifacts
--------------------
The maven pom.xml contains the version of the product. During normal builds, this version number ends with a `-SNAPSHOT`, for example `1.1.0-SNAPSHOT`.  At release time, this number is changed to drop the `-SNAPSHOT` and the final release of that version is performed.  Once that is done, the version is incremented and the `-SNAPSHOT` is added, for example `1.1.1-SNAPSHOT`.  We typically use exactly 3 integer values for versions.

Final binary artifacts must have version numbers that align with the version of the release, which also aligns with the version of the software and the release notes.  We accomplish this using the maven plugin named `maven-resources-plugin`.  With this plugin, developers add macros to source code which contain version information, and at build time, these files
are configured to be copied using the `maven-resources-plugin`.  During the copy operation, the plugin filters the macros and populates the values with the current version that is being built.

There are details on versioning with maven here: https://s3-us-west-2.amazonaws.com/shared-external/videos/BuildVideos.html

    Requirement:  All binary artifacts that are produced by a build that can contain version information must be configured to automatically populate the version of the release that is being built.

Tagging Software During Release
-------------------------------
Once a release build has been successfully performed, the software in Git must be tagged so that it is possible to reproduce the exact source that was used to produce the build.  This step is generally handled by the Jenkins job that is performing the build.  For more information, see Jenkins release jobs: http://10.244.92.221:8080/

    Requirement: All release jobs must tag the software that was used to create the release.

Archive Artifacts
-----------------
Once a build has been successfully performed, the artifacts that have been produced must  be archived in the Beckman artifact repository.  We use the `maven-s3-wagon` maven build extension to push artifacts to the Beckman artifact repository in the `deploy` phase of  a build.  

    Requirement: All release jobs must archive the artifacts reproduced to the Beckman artifact repository using maven.

Generation of Release Notes
---------------------------
Release notes are a key element of any released software product.  Release notes tell the user what has changed in the product since the last release.  Release notes are not a list of commit messages from Git - those are messages for another developer.  Maven enables release notes using the `maven-site-plugin`. When a release is successful,  release notes are compiled into an HTML web site and archived on the Beckman artifact  repository. 

A tutorial for how to do this can be found at: https://s3-us-west-2.amazonaws.com/shared-external/videos/BuildVideos.html

    Requirement: All release jobs must automatically create and archive release notes on the Beckman artifact repository.



