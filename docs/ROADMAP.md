# Product Roadmap

This document lists some future development paths that could be followed for each of the core components of the BMAChatbot

## NLParser

- Active learning approach to interactively train the parser as to the kind of formulae that the user can submit
- Accept phrases with infix unary operators ie: "a is always 1"
- Enhance the error recovery to perform probabilistic token replacement as well as token skipping
- Identify unknown variables in formula

## Model Knowledge

- Support activity levels like "a is off" or "a is highly active"
- Allow user to work with multiple models and persist formula history of each model seperately

## Knowledge base

- Linking general queries such as 'What is LTL' with the tutorials, and providing the user with an option to see a descriptive answer or open a tutorial (if available)

## UI Integration

- Provide the chat bot within the browser, so user does not have to jump between Skype and BMA UI
- Real-time synchronization of model changes on the UI with the bot
- Provide user with best possible changes within a model to satisfy a formula

## Storage

- Storage (conversation state and uploaded/generated models) currently does not expire, which should be implemented before going into production