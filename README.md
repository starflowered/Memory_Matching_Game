# VUFORIA SETUP:

1. Go to https://developer.vuforia.com/downloads/sdk
2. Log in with Vuforia account
3. Download Vuforia Engine 10.15 to "Projectfolder/Assets/Vuforia"\
   **!!! DOWNLOAD LOCATION FOLDER IS IMPORTANT !!!**
4. In Unity: Double-click on vuforia Unity package inside "Projectfolder/Assets/Vuforia" and then click on "Import"
	- if unity suggests to enter safe mode please do so and look in the section "Issues" for more information 
5. Click "Update" if there is a popup



# ART FOLDER:

Please put every Art Asset into the "root/Assets/ART" folder !
	
Please follow a naming convention !



# SOUND FOLDER:

Please put every Sound Asset into the "root/Assets/SOUND" folder !
	
Please follow a naming convention !



# TESTING

1. In Unity, go to "File > Build Settings" (or press ctrl + shift + B)
2. Make sure that all scenes you need in the build have a build index
3. Make sure that "Android" is selected in the "Platform" list on the left
4. On the right, click on the drop down menu next to "Run Device" and select your device\
	&rarr; if it cannot be found look at ISSUES down below
5. Click on "Build And Run", select the "root/Assets/Build" folder, give the application a name and click on "Save"\
	&rarr; if you do not have a "root/Assets/Build" folder, create one (naming is important)



# ISSUES

### Errors after cloning repository and integrating Vuforia:

**Please Enter Safe Mode when Unity suggests so, then do the following:**

1. Make sure that you have a file called "com.ptc.vuforia.engine-10.15.4.tgz" inside you "root/Packages" folder
	- If not, please ask a team member so they can send it to you
	- Otherwise open a new Unity project and follow the Vuforia steps, then copy the file into this project
2. Close and reopen the project in Unity (it should not suggest safe mode anymore)


**If entering safe mode was not suggested by Unity:**

1. Delete root/Packages/packages-lock.json
2. Go back to Unity and let it recreate it
3. Delete root/Packages/manifest.json
4. Go back to Unity and let it recreate it

**IMPORTANT: DO NOT DELETE BOTH FILES AT THE SAME TIME**



### Phone cannot be found:

**FIRST: CHECK DEVICE (CONNECTION)**

- Check if it is connected with the PC
- If connected, there should be a notification on your device, where you can select how the device will be connected to the PC. Select "Allow data transfer".
- Check if the developer mode is activated on your device. If not, look for the device's build number and tap it until you are in developer mode.
- Go to the developer settings and enable "USB Debugging"

**ELSE**

1. Go to Project settings > Player
2. In the Android tab go to "Other Settings"
3. Under "Rendering", disable "Auto Graphics API" and remove "Vulkan" from the list that pops up
4. Under "Identification", set the "Minimum API Level" to "Android 8.1 'Oreo' (API level 27)"\
	&rarr; make sure your device fulfills this requirement (if not, Vuforio will not work with your device)
5. Check if it works now, if not, continue with step 6
6. Under "Configuration", check the selected "Scripting Backend". If it is set to "Mono", continue with step **6a)**, else continue with step **6b)**\
	**a)** Set the "Scripting Backend" to "IL2CPP" and under "Target Architectures", deselect "ARMv7" and select "ARM64". Try to find your device again.\
	**b)** Set the "Scripting Backend" to "Mono" and under "Target Architectures", deselect "ARM64" and select "ARMv7". Try to find your device again.







