# Boiler Demo
This demo was developed to demonstrate an alarm click in FactoryTalk® Optix pulling up a rung of code in Studio 5000 Logix Designer® using ThinManager® Logix PinPoint. This will connect via OPC-UA to the demo server provided by the UA Foundation [link](https://www.unified-automation.com/downloads/opc-ua-servers.html), choosing **OPC UA ANSI C Demo Server (Windows)**, and a Rockwell PLC running the Boiler Demo code that can be found in `ProjectFiles\LogixCode`

## Cloning the repository
There are multiple ways to download this project, here are two of them:

### Clone repository in FactoryTalk Optix(R)
1. Click on the green `CODE` button in the top right corner
2. Select `HTTPS` and copy the provided URL
3. Open FT Optix IDE
4. Click on `Open` and select the `Remote` tab
5. Paste the URL from step 2
6. Click `Open` button in bottom right corner to start cloning process

### Download ZIP
The Download ZIP button provided bi GitHub may be used to download a .zip copy of the repository.

## Preparing the demo
1. Download, install, and run the OPC-UA Server from the link above on the same computer hosting your FactoryTalk Optix application. The OPC UA ANSI C Demo Server is what is used for this project.
2. Download the PLC Code from `ProjectFiles\LogixCode` to a PLC (FactoryTalk Echo can be used).
3. Create a PinPoint Event in the ThinManager server that executes on a specific terminal (event type will be set to PinPoint, target terminal must be specified).
4. Generate an API key from the ThinManaager server with the appropriate permissions to connect to the server and execute a ThinManager Event.
5. Modify the example code in the TMLogixPinPoint.cs file to use the updated API key for the "x-api-key" header and also update the IP Address of the ThinManager server in the url_ variable.

## Disclaimer

Rockwell Automation maintains these repositories as a convenience to you and other users. Rockwell Automation reserves the right at any time and for any reason to refuse access, to edit, or remove content from this Repository. You acknowledge and agree to accept sole responsibility and liability for any Repository content posted, transmitted, downloaded, or used by you. Rockwell Automation has no obligation to monitor or update Repository content

The examples provided are to be used as a reference for building your own application and should not be used in production as-is. It is recommended to adapt the example code based on your project/needs while observing the highest quality and safety standards.

The following list, while not inclusive, are pieces of software that require a paid license or subcription to run in production:
- ThinManager
- ThinManager Logix PinPoint
- FactoryTalk® Optix
- FactoryTalk® View SE
