Swate
=====

> **Swate** - something or someone that gets you absolutely joyed([Urban dictionary](https://www.urbandictionary.com/define.php?term=swate))

**Swate** is a **S**wate **W**orkflow **A**nnotation **T**ool for **E**xcel.

_This project is in an exerimental state._



The aims of Swate are:

1. Provide an easy way to annotate experimental data in an application (excel) that every wet lab scientist is familiar with
2. Provide a way to create computational workflows that start from raw data and end with results.
3. Create ISA and CWL compatible data models from the input
4. Provide an interface for remote execution of the defined workflows

Check the state of the [minimal POC milestone ](https://github.com/nfdi4plants/Swate/milestone/1) for the current state of features.

Discuss
-------

Extensive documentation of Swate will be setup when the [development process of a minimal POC]() is complete.

Please use Github issues to track problems, feature requests, questions, and discussions. Additionally, you can get in touch with us on [Twitter](https://twitter.com/nfdi4plants)

Develop
-------

#### Prerequisites:

 - .NET Core SDK at of at least the version in [the global.json file](global.json)
 - Node.js with npm/npx
 - connections from excel to localhost need to be via https, so you need a certificate and trust it. [office-addin-dev-certs](https://www.npmjs.com/package/office-addin-dev-certs?activeTab=versions) does that for you (should be a fake target pretty soon (TM))
 - You may need a loopback exemption for Edge/IE (whatever is run in your excel version)

This project uses the [SAFE Stack](https://github.com/SAFE-Stack) to create a website that uses [office.js](https://github.com/OfficeDev/office-js) to interop with Excel.

The file [OfficeJS.fs](src/Client/OfficeJS.fs) contains Fable bindings autogenerated from the office.js typescript definitions.

to debug the AddIn locally, use the build target `OfficeDebug`:

`fake build -t OfficeDebug`

this will launch an Excel instance with the AddIn sideloaded.

However it is currently pretty hard to attach a debugger to the instance of edge that runs in
the Excel window. You can circumvent this issue by additionally testing in Excel online:

 - open Excel online in your favorite browser
 - create a blank new workbook (or use any workbook, but be warned that you can't undo stuff done via Office.js) 
 - Go to Insert > Office-Add-Ins and upload the manifest.xml file contained in this repo
    ![Add In Upload](docsrc/files/img/AddInUpload.png)
 - You will now have the full debug experience in your browser dev tools.

Alternatively, you can debug all functionality that does not use Excel Interop in your normal browser (the app runs on port 3000 via https)