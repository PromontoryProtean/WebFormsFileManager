# WebFormsFileManager
An ASP.NET Web Forms based file manager.

This project began with my need to evaluate ImageProcessor, and grew to become a proof-of-concept to demonstrate how various open source projects can be combined to create a web-based file manager. It can be integrated into more complex applications such as content management systems, corporate intranets etc.

The open source projects that I have integrated are:
- ImageProcessor (https://github.com/JimBobSquarePants/ImageProcessor)
- Plupload (https://github.com/moxiecode/plupload)
- Bootstrap (https://github.com/twbs/bootstrap)
- js-cookie (https://github.com/js-cookie/js-cookie)
- Font Awesome (https://github.com/FortAwesome/Font-Awesome)
- jQuery (http://jquery.com)

Some of the features are:
- Detail and Tile views.
- Multi-file upload with progress monitoring.
- Public and secure directories.
- Bulk delete.

#Configuration
You can set the UserUploadDirectory in the appSettings section of the Web.config file. The secure file directory is hard coded to App_Data/secure-files.

#Security
Make sure to set the path to the UserUploadDirectory in the Web.config file to make sure that users cannot upload and execute malicious files.
