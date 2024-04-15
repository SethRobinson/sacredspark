call ..\base_setup.bat
:APP_NAME must match the directory name
SET APP_NAME=sacredspark
SET APP_PATH=%cd%
:Package names are used in Android builds.  It needs to match the Unity project setting
SET APP_PACKAGE_NAME=com.rtsoft.%APP_NAME%

:If website file transfer batch files are used, these should be set here (or in their .bats)
set _FTP_USER_=rtsoft
set _FTP_SITE_=rtsoft.com
SET WEB_SUB_DIR=sacredspark

:Applicable if UploadLinux64HeadlessRSync.bat and friends are used (note: "Server" gets appended to the name as well, and "BetaServer" for beta versions)
SET LINUX_SERVER_BINARY_NAME=sacredspark