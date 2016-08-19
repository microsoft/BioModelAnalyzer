# Architecture

Bot = node.js server + static web resources
- server exposes REST API which is invoked by bot framework service
- static web resources directly exposed by IIS when deployed (local development: exposed by node.js server)
- deployed on Azure as App Service

Bot uses external services:
- LUIS for natural language processing
- Azure Blob Storage for storing BMA model files uploaded by users
- Bing Spell Check API for cases when LUIS did not understand the input
- BMA backend for running simulations within a conversation



