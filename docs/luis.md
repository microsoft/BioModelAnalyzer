# LUIS: BMA Chat Bot
Below is the documentation for the entities and intents used to build the LUIS model.
Use examples shown to invoke intents in luis.ts

## Intents
#### AboutBot
Recognises user greeting the bot, or asking for help, returns ABOUT_BOT. 

*Example: hello*

#### AboutSimulations
Recognises user asking about simulations, returns ABOUT_SIMULATIONS. 

*Example: What kind of simulations can you find?*

#### Back
Chosen intent for when user requires to go back, may be useful for tutorials. Users current conversation is stored, 'Back' is invoked and data from the conversation is retrieved. 

*Example: Please go back*

#### ExplainOp
Knowledgebase query, gives user an explanation of each operator, Required param: Operator, returns ABOUT_{OPERATOR}.

*Example: How can I use the until operator in my model?*

#### ListTutorial
Chosen intent when user asks for tutorials, returns list of all available files in 'docs', allows user to choose  

*Example: List all the tutorials available*

#### LTLQuery
Recognises users query as an LTL query, this is later parsed to return a formula, required param: Query 

*Example: I need to see ras equal to 1 and notch equal to 1 sometime later*

#### None
No intents are recognised, and a default intent is called to alert the user, returns UNKNOWN_INTENT

*Example: This is irrelevant* 

#### OperatorExample
Knowledgebase query, gives user an example of each operator, Required param: Operator, returns EXAMPLE_{OPERATOR}

*Example: Do you have an example that shows me how to use the implies operator?*

#### OperatorInteractions
Knowledgbase query, explains and provides examples of different interaction possibilities.

*Example: How can I use eventually always* 

#### SelectTutorial
Chosen intent when user asks for a specific tutorial 

*Example: Are there any tutorials that can explain stability*

#### UploadedModel
Recognises user asking for their model, and returns model name

*Example: Can you check the model that was uploaded*

## Entities
#### Operator
Recognises operator, used for 'OperatorExample', 'ExplainOp'

#### Lookup
Default entity, can be used within any intent

#### Query
Recognises query, used for intent 'LTLQuery'