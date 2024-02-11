# Demo Video (Turn Sound On)

In the following Demo Video you get to experience both players perspectives! **Remember** that in a typical game the players would have **seperate** senses only experiencing **either** sound or video:

https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/82c6396c-9ad9-4ee1-bbff-638bb4d2df56

# Overview

Memory Matching Game is intended for young children to learn about their favourite farm animals!

It is a *multi-sensory* learning experience that aims to improve spatial thinking skills and the motoric development of young children.

Two players play the game asymmetrically, each having different capabilities and roles within the game. One plays using Headphones and **Audio** Cues, while the other has a tablet and follows **Visual** cues:

<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/1641be1b-b16e-43b6-a034-0549c3ca77e8" width="300">
<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/d52ad15f-44d3-4373-b89b-9ec603a146fa" width="300">  

Each Player plays with a randomized set of Memory Cards (featured with tracking Markers) and tries to match their cards with the other player. 

What follows is an interactive and fun learning experience for children integrating different stimuli!

# Game Loop
After a playing field was set up the game consists of 3 main stages:

<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/8427f4b6-e4c5-481d-9598-15f7bd6825d2" width="600">

## Lock
Here players turn around their respective game cards to choose which they want to compare and lock these choices in. The **Audio** player gets a cue on their headphones while the **Visual** Player gets to see their animal on the tablet. Neither knows about the information of the other player.

<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/9ecee535-e735-46f4-934c-db1df5cc976e" width="600">

## Discuss
The players are now encouraged to discuss their respective (auditory or visual) information:

<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/47d57c88-120e-46b6-80ba-43e59dc0c63e" width="600">

## Compare
Lastly the players push their cards together to compare if its a match or not:

<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/8ec986e0-1421-4183-9c7b-56e55fa9af58" width="300" height="300">
<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/2f3229dc-8bea-4f62-97b6-8b201884c435" width="300" height="300">

# Evaluation

This project was evaluated with parents of our target audience. Some of the key lessons we learned during this evaluation are:


<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/0c3d3fe9-4d1e-41c1-a0b4-2e5ca81f745a" width="300">
<img src="https://github.com/Pydes-boop/Memory_Matching_Game/assets/59734957/c8e0199d-a32e-4d34-879f-d8d83090eb87" width="300">

<details>
  <summary>Survey Tools</summary>
  
1. NASA-TLX (perceived workload evaluation)
2. Modified SUS (System Usability Scale) for games
3. Sensore Workload and Stimulation Questionnaire
4. Perceived Usability as a teaching tool

</details>

<details>
  <summary>Development and Setup</summary>
  
### VUFORIA SETUP:

1. Go to https://developer.vuforia.com/downloads/sdk
2. Log in with Vuforia account
3. Download Vuforia Engine 10.15 to "Projectfolder/Assets/Vuforia"\
   **!!! DOWNLOAD LOCATION FOLDER IS IMPORTANT !!!**
4. In Unity: Double-click on vuforia Unity package inside "Projectfolder/Assets/Vuforia" and then click on "Import"
	- if unity suggests to enter safe mode please do so and look in the section "Issues" for more information 
5. Click "Update" if there is a popup

### TESTING

1. In Unity, go to "File > Build Settings" (or press ctrl + shift + B)
2. Make sure that all scenes you need in the build have a build index
3. Make sure that "Android" is selected in the "Platform" list on the left
4. On the right, click on the drop down menu next to "Run Device" and select your device\
	&rarr; if it cannot be found look at ISSUES down below
5. Click on "Build And Run", select the "root/Assets/Build" folder, give the application a name and click on "Save"\
	&rarr; if you do not have a "root/Assets/Build" folder, create one (naming is important)

### ISSUES

#### Errors after cloning repository and integrating Vuforia:

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



#### Phone cannot be found:

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

</details>
