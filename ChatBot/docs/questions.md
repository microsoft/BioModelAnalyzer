# Adding static questions

General queries from the user are stored as static questions and answers within the bot, with the help of the LUIS model.

## Training a new question: LUIS

We will be using the example 'What is the BMA':
- Open the [LUIS](https://www.luis.ai/) model using the bma chat bot credentials
- Create a new intent with a descriptive name, that is consistent with intents that have already been created, e.g. AboutBMA
- Enter an example e.g. 'What is the BMA?', save the intent and submit the utterance
- Continue to add different utterances the user may enter, and submit them as the AboutBMA intent
- Train the model and then update the published application

For more information on how LUIS can be used click [here](https://www.luis.ai/Help) for a tutorial
and [here](https://www.microsoft.com/cognitive-services/en-us/luis-api/documentation/home) for documentation.

## Adding question to BMA chat bot 

- Open `/src/dialogs/strings.ts` to add your answer with a descriptive name, e.g. `ABOUT_BMA = 'insert description here'` 
- Open `/src/dialogs/luis.ts` to add the new intent handler using the format below:

```js
    matches('AboutBMA', (session) => {
        session.send(strings.ABOUT_BMA)
    })
```
