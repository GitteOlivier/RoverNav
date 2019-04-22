# Project overview
The Mars Rover is a motor vehicle that travels across the surface of Mars to examine territory, can be directed to positions, and can interpret weather conditions (www.wikipedia.org/wiki/Mars_rover). The purpose of this project is to implement a simplistic navigation system to remotely control the rover, whilst investigating various aspects of software development (arhcitectures, designs, techniques, languages). Basically, an onging, fun way to explore different software development territories, almost like the rover itself. This project will start on Earth, where the navigation commands are transmitted to the rover, and will progress to end on Mars, on the rover itself, where the commands are interpreted and executed. 

# Problem statement
- Mars's surface is divided into zones. 
- Each zone is a two-dimensional grid of squares that have been surveyed ahead of time and is deemed safe for exploration within the bounds of the zone.
- Each zone represent a grid of square areas, and has a minimum and maximum cartesian coordinate (e.g., (-1,0) and (5,5)).
- The rover understands cardinal points and can face either East (E), West (W), North (N) or South (S).
- The rover understands three commands:
    + (M) - Move one space forward in the direction it is facing.
    + (R) - Rotate 90 degrees to the right.
    + (L) - Rotate 90 degrees to the left.
- Currently, due to the communication transmission delay between Earth and Mars, only one command set can be sent to the rover at a time.
- A command set comprises of the following: 
    [xmin, ymin] [xmax, ymax] [xstart, ystart, hstart] [commands]
    
    where
      [xmin, ymin] is the minimum cartesian coordinate of the zone's boundary.
      [xmax, ymax] is the maximum cartesian coordinate of the zone's boundary.
      [xstart, ystart, hstart] is the starting position and heading of the rover.
      [commands] is a list of commands, directing the rover where to go.

    example
      [0, 0] [8, 10] [1, 2, E] [MMLMRMMRRMML]
        The zone comprises of 80 blocks.
        The rover starts at x = 1, y = 2 and faces East.
        The rover would land up at [3, 3, S].

- The rover returns the resulting location, and the result code of the command set in the following format:
      [xpos, ypos, hpos] [moduleId, resultcode]

    where
      [xpos, ypos, hpos] is the resulting position and heading of the rover.
      [moduleId, resultcode] is the module ID and resultcode 

- If an error occurred during the execution of a command without completing the list of commands, the rover will discard the remainder of the commands and return the error code with the resulting position.


# Development philosophy
Less is more. A minimalist software development approach without compromising the system requirements ensures that software achieves better code coverage, facilitates testing, is more maintainable, and costs less. More effort is required up front especially during system design, where alternate approaches to problems can either simplify the system, or lead to complexities that will filter down to all of the system components.

Implement real-world, practical solutions. Do not anticipate or envision problems, behaviours, functionalities that are not known, as unnecessary code may be implemented. Rather implement a solid and robust solution based on user requirements, and when deployed, use the feedback to improve the system.

Use design patterns, don't use design patterns. Use design patterns mindfully to solve specific system needs. Do not use design patterns for the sake of using or experimenting with design patterns, as this may add unnecessary complexities to the system at the cost of maintainability. But... in this project, experimenting is in order, as long as it ends with a conclusion, and is refactored where necessary!

Test, test, test. Implement meaningful automated tests (including unit testing) focusing on the functional behaviour (requirements-based) of the system as a mimimum. Test inputs thoroughly and handle exceptions gracefully. 


Error handling is important. Users should always be aware of the outcome of a use case, and be able to correct for errors where possible, e.g. if an invalid file was specified. Other errors need to be logged to facilitate system diagnostics. It is important to be able to pinpoint the origin and time of an error, and the use case that caused the error. Part of this project (onging) will be to investigate the benefits of using exception handling vs result codes as there are various opinions about these topics.

The selected programming language(s) must compliment the system. 


# System overview

MissionPlanner (Earth) ->  Communication Protocol  ->  NavReceiver (MarsRover)

The MissionPlanner on earth determines what the mission objective is for the rover, and calculates navigation commands that the rover must execute. These commands are tested before they are authorised for transmission, to ensure that the rover will not move out of the zone bounds. 

The navigation commands are transmitted to the rover in the form of command sets, via a secure communication protocol. 

The NavReceiver receives the command sets and sends the commands to the rover guidance system for execution.


# Software design

# MissionPlanner
The first iteration of the MissionPlanner will be command-line based and use a text file containing a command set as input. The command set will be verified and the resulting rover location will be displayed to the console. 
In future iterations, the command sets can be obtained through other mechanisms than files, therefore consideration should be given to abstract the interface. The MissionPlanner will run on a personal computer or mobile application. The initial programming language is C#. As the command set will be transmitted and executed on a remote processor, care has to be taken to ensure that there are no byte-alignment issues and that the data size and endianness will be interpreted the same on the sending and the receiving platforms.


