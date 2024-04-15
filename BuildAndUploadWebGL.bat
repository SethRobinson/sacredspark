set NO_PAUSE=1
SET BUILDMODE=RELEASE
call BuildWebGL.bat

call UploadWebGLRsync.bat
pause