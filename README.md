# Autonomous Intersection Simulation & A.I. 
Developing and Simulating Self-Driving Car A.I. Without Need for Traffic Lights 

## Summary
- The purpose of this project was to create a computer simulation of a four-way intersection with no traffic controls (stop signs or traffic lights) and then develop an autonomous artificial intelligence such that the self-driving cars regulate their speed and pass through the intersection without any collisions.
- This 2D simulation was created entirely from scratch in Visual Studio 2017 using C#, and leveraged the MonoGame framework, an improvement to Microsoft's discontinued XNA 4.0 framework.
- The physics and mathematical details of the simulation were created from scratch specifically for this simulation, with acceleration and deceleration physics derived from data on real world tests of the Tesla Model S.
- The simulation is incredibly flexible and gives the operator extremely granular control over all characteristics of the simulation. This is thanks to the Vars.cs file which contains nearly 50 public global constants that both the simulation and the AI have access to. This means that the even the thickness and length of the dashed lines on the road can be modified. By drawing the simulation dynamically based on these constants, a myriad of different scenarios can be simulated.
- What separates this AI from any other existing solutions out there, is that is fully decentralized and autonomous. There is no central server controlling all the vehicles. Rather, each vehicle makes its own decisions using basic utilitarian  values such that the efficiency and safety of the intersection as a whole is the number one priority. By making the AI decentralized, there is no risk of having every car on the road crash when there is a failure with the central server. 
- This was accomplished by leveraging what is known as "swarm intelligence" - all the cars intercommunicate such that they knew exactly what all the other car's current states are and can accurately predict where collisions would occur and then decide how to avoid them.   

<br></br>

## Image Gallery

<br>


### The Autonomous Intersection in Action!  
![demo of intersection with 4 lanes gif](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/4_lane_demo.gif)

<br>

### Threading the Needle 
![simulation with 1 lane](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/4_lane_1.jpg)

<br>

### High-Level Flowchart of the Cars' AI 
![high-level ai flowchart](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/flowchart.jpg)

<br>

### Simulation with 1 Lane  
![simulation with 1 lane](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/1_lane_1.jpg)

<br>

### Simulation with 2 Lanes  
![simulation with 2 lanes](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/2_lane_1.jpg)

<br>

### Simulation with 3 Lanes  
![simulation with 3 lanes](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/3_lane_1.jpg)

<br>

### Simulation with 4 Lanes
![simulation with 4 lanes](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/4_lane_2.jpg)

<br>

### Demo of Simulation with 1 Lane 
![demo of intersection with 1 lanes gif](https://github.com/a-dubs/autonomous-intersection/blob/main/image_gallery/1_lane_demo.gif)

<br>

<br>

## Project's Display Board From ISEF 2019
![Project Board Displayed at ISEF 2019](https://github.com/a-dubs/autonomous-intersection/blob/main/Display%20Board%20for%20ISEF%202019.png)

<br>

## Demo Video
[Demo Video on Youtube of Various Runs with 1-4 Lanes](https://youtu.be/VOxNxQTsYMI)


<br>

<br>

<br>


## Project Metadata

**Project Status** : Active  
**Project Progress** : Completed  
**Project Dates** : Oct '17 - Present  

// portfolio.alecwarren.com position priority = 9 (-1 is lowest, 0 is default, 10 is highest)

