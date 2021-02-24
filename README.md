# autonomous-intersection  
### Developing and Simulating Self-Driving Car A.I. Without Need for Traffic Lights  
- The purpose of this project was to create a computer simulation of a four-way intersection with no traffic controls (stop signs or traffic lights) and then develop an autonomous artificial intellegence such that the self-driving cars regulate their speed and pass through the intersection without any collisions.
- This simulation was created in Visual Studio 2017 using C#, and leveraged the MonoGame framework, an improvement to Microsoft’s discontinued XNA 4.0 framework.
- The physics and mathematical details of the simulation were created from scratch specifically for this simulation, with acceleration and deceleration physics derived from data on real world tests of the Tesla Model S.
- The simulaion is incredibly flexible and gives the operator extremely granular control over all characteristics of the simulation. This is thanks to the Vars.cs file which contains nearly 50 public global constants that both the simulation and the AI have access to. This means that the even the thickness and length of the dashed lines on the road can be modified. By drawing the simuation dynamically based on these constants, a myriad of different scenarious can be simulated.
- What separates this AI from any other existing solutions out there, is that is fully decentralized and autonomous. There is no central server controllin all the vehicles. Rather, each vehicle makes its own decisions using basic ulitarian values such that the efficiency and safety of the intersection as a whole is the number one priority. By making the AI decentralized, there is no risk of having every car on the road crash when there is a failure with the central server. 
- This was accomplished by leveraging what is known as "swarm intelligence" - all the cars intercommunicate such that they knew exactly what all the other car's current states are and can accurately predict where collisions would occur and then decide how to avoid them.   
  
  
#### Basic Flowchart of AI:
![Basic flowchart of the AI used in this project](https://github.com/AlecWarren19/autonomous-intersection/blob/main/flowchart.png)
