# Adding static questions

General queries from the user are stored as static questions and answers within the bot, with the help of the LUIS model.

## Training a new question: LUIS

We will be using the example 'What is the BMA' :
- Open the [LUIS](https://www.luis.ai/) model using the bma chat bot credentials
- Create a new intent with a descriptive name, that is consistent with intents that have already been created, e.g. AboutBMA
- Enter an example e.g. 'What is the BMA?', save the intent and submit the utterance
- Continue to add different utterances the user may enter, and submit them as the AboutBMA intent
- Train the model and then update the published application

* For more information on how LUIS can be used click [here](microsoft.com/cognitive-services/en-us/LUIS-api/documentation/home) 

## Adding question to BMA chat bot 

- Open strings.ts to add your answer with a descriptive name, e.g. `ABOUT_BMA = 'insert description here'` 
- Open luis.ts to add the new intent handler using the format below:

```
    matches('AboutBMA', (session) => {
        session.send(strings.ABOUT_BMA)
    })
```



