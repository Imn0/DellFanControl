# DellFanControl
Windows program to control fan speeds on dell laptps using [DellFanManagement](https://github.com/AaronKelley/DellFanManagement). 
Download release (or compile soruce), if you want to start it with windows you have to create task with task scheduler, remember to run with highest privilages as the app needs administrator access. 



Configure behaviour with `app_config.json`

Dell has only 3 avaiable fan speeds, off, medium speed and full speed. Here is a table how that is converted to speeds in the config file. 
| App value | fan 1 | fan 2 |
|-----------|-------|-------|
|  Off      |  off  |  off  |   
|  VeryLow |  off  | medium   |      
| Low       |  medium |medium  |
| Medium    | medium | full speed | 
| High      | full speed | full speed |
