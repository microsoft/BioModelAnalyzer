# LUIS: BMA Chat Bot
Below is the documentation for the entities and intents used to build the LUIS model.
Use examples shown to invoke intents in luis.ts

## Intents
<b>*AboutBot*</b> </br>
Recognises user greeting the bot, or asking for help. </br>
@return ABOUT_BOT </br>
Example: hello </br>

<b>*AboutSimulations*</b> </br>
Recognises user asking about simulations </br>
@return ABOUT_SIMULATIONS </br>
Example: What kind of simulations can you find? </br>

<b>*Back*</b></br>
needs implementation </br>
Chosen intent for when user requires to go back, may be useful for tutorials </br>
Users current coversation is stored, 'Back' is invoked and data from the conversation is retrieved. </br>
Example: Please go back </br>

<b>*ExplainOp*</b> </br>
Knowledgebase query, gives user an explaination of each operator </br> 
Required param: Operator </br>
@return ABOUT_{OPERATOR} </br>
Example: How can I use the until operator in my model? </br>

<b>*ListTutorial*</b></br>
Chosen intent when user asks for tutorials. </br>
Returns list of all available files in 'docs', allows user to choose</br>
Example: List all the tutorials available </br>

<b>*LTLQuery*</b></br>
Recognises users query as an LTL query, this is later parsed to return a formula </br>
Required param: Query </br> 
Example: I need to see ras equaal to 1 and notch equal to 1 sometime later </br>

<b>*None*</b></br>
No intents are recognised, and a default intent is called to alert the user </br>
@return UNKNOWN_INTENT </br>
Example: This is irrelevant </br>

<b>*OperatorExample*</b></br>
Knowledgebase query, gives user an example of each operator </br>
Required param: Operator </br>
@return EXAMPLE_{OPERATOR} </br>
Example: Do you have an example that shows me how to use the implies operator? </br>

<b>*OperatorInteractions*</b></br>
needs implementation </br>
Knowledgbase query, explains and provides examples of different interaction possibilities. </br>
Example: How can I use eventually always </br>

<b>*SelectTutorial*</b></br>
needs implementation (may not be needed) </br>
Chosen intent when user asks for a specific tutorial </br> 
Example: Are there any tutorials that can explain stability </br>

<b>*UploadedModel*</b></br>
Recognises user asking for their model, and returns model name </br>
Example: Can you check the model that was uplaoded </br>

## Entities </br>
<b>*Operator*</b></br>
Recognises operator, used for 'OperatorExample', 'ExplainOp' </br>

<b>*Lookup*</b></br>
Default entity, can be used within any intent </br>

<b>*Query*</b></br>
Recognises query, used for intent 'LTLQuery' </br>