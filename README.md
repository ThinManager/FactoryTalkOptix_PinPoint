# Boiler Demo
This demo was developed to show some features of Factory Talk Optix. This will connect via OPC-UA to the demo server provided by the UA Foundation [link](https://www.unified-automation.com/downloads/opc-ua-servers.html) and a Rockwell PLC running the Boiler Demo code that can be found in `ProjectFiles\LogixCode`

## Cloning the repository
There are multiple ways to download this project, here are few of them

### Download Release
1. Click [here](https://github.com/SalesAndApplications/BoilerDemo/releases) and download the latest ZIP file

### Clone repository
1. Click on the green `CODE` button in the top right corner
2. Select `HTTPS` and copy the provided URL
3. Open FT Optix IDE
4. Click on `Open` and select the `Remote` tab
5. Paste the URL from step 2
6. Click `Open` button in bottom right corner to start cloning process

### Download ZIP
The Download ZIP button provided bi GitHub may not work as intended, please download the zip from the `Release` section

## Preparing the demo
1. Download the OPC-UA Server from the link above
2. Download the PLC Code from `ProjectFiles\LogixCode` to a PLC (FT Echo is fine)
3. Execute the demo

## Description
This demo was made to demonstrate interoperability between different communication drivers, three aliases in the MainPage are used to interface those drivers, when clicking a button, the command is also sent to the other driver to trigger the water flow.

Widgets has been prepared using images from the TemplateLibrary

## Script for presentations
When exposing this demo, the typical procedure is described in the [dedicated NetLogic](./ProjectFiles/NetSolution/DemonstrationScript.cs)
