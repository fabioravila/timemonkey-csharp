# timemonkey-csharp
A simple application that watches the time spent on applications and websites on your desktop.

# Inpiration and Purpose
The inspiration for performing this software came from a number of other performance software that perform the same task, such as Rescue Time, Hubstaff, and TimeDoctor.

I always look for new projects to train my programming. As I work with C # again I will do it all in C #, though and use WinAPI with PInvoke.

The goal is to create a functional application, which performs a usage log on the software, windows and urls.

# Goals
* Map time spent on each Windows operating system window
* Map the conditions of use of the computer, based on the events of the keyboard and mouse
* Identify behavior by mouse and keyboard, like stress, and irritability


# Milestone
[] Group and organize codes and structs for for WinAPI Interop.
[] Create a simple and rich classes for Mouse ans Keyboard Monitor
[] Create simple and direct classes for window monitors
[] Identify PC presence, afk and lock screen
[] Criar a RepositoryProvider to allow the implementation of a storage and sending the collected information
[] Tray application with some config, alerts and info forms
[] Always alive app, to prevent accidentally close

# OpenSource Codes
Several codes and have been taken from internet examples, github and stackoverflow responses, I will try to relate them all here.

github.com/gmamaladze/globalmousekeyhook - For mouse and keyboard hook
github.com/gmamaladze/globalmousekeyhook - For mouse and keyboard hook extensions and rich details