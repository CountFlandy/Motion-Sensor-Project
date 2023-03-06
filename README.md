# Motion-Sensor-Project
A small application that takes motion input from a webcam and plays a soundfile when motion is detected. Saves up to 10 of the motion detected images, as well as supports a large variety of sound files.

Mostly finished initial itteration of this project, started because I wanted to do something silly to torture myself via motion sensor

**Its current features are as follows:**

1: Detects if there is a Webcam

2: Creates and Searches through a premade directory to make a list of all soundfiles the user has placed in said directory

3: After all the setup is done, it uses the webcam to detect if any pixels have changed, and based on the thresholds set it will then trigger a sound file to be played

4: Keeps a total of 10 of the Motion Captured Images that it detected motion in

Small Disclaimer: Honestly I have no idea what the motion capture system is, I just threw it together from previous unpublished projects and memory.
I know it likely has a lot of flaws, but for a small project like this it works okay for now. 

**It could however use some improvement:**

1: Currently it just hangs when theres no motion (Thanks to  the wait(true) line of code) - This should be improved first

2: It could use some actual commands in text to tweak settings and store them in a file somewhere

3: Would be nice to have some sort of UI for all this, along with a UI for displaying the output of the webcam currently

4: Theoretically if theres two webcams connected to the computer it will likely use both of them at the same time. Or crash. Untested but likely needs attention.

5: Motion Sensor code kept detecting motion in a reflection that was still and unmoving. Likely a deep rooted problem but would be nice to fix.

Other contributions welcome, or not. Feel free to use my code for your own personal projects if you so desire. Suggestions welcome. 


**Credits:**

1: Initial mockup to save time was done by ChatGPT. All other code was written by me.

2: I used AForge Library for Image processing (http://www.aforgenet.com/framework/) - Because otherwise I'd have torn my hair out trying to figure that out.
