[trainee-final-ppt]: https://microsoft-my.sharepoint.com/personal/t-mariec_microsoft_com/Documents/BMAChatBot%20shared/2016-09-09%20BMAChatBot%20final%20presentation.pptx?d=w44cde67cfd8c4c64a73f231fde3199b4

# Architecture

![Architecture overview](img/architecture.png)

See also the [final traineeship presentation][trainee-final-ppt].

The chat bot is a Node.js server plus static web resources for the tutorials (see /public folder).
During local development the static web resources get served by the same Node.js server for convenience.
When deployed on Azure as an App Service the static web resources get served directly via IIS (see /web.config).

As seen on the image above, the user does not talk directly to the bot service
but rather to the Bot Framework service, which then talks to the bot.
The Bot Framework provides an abstraction over the different communication channels like Skype, Slack, Facebook, IFrame etc.
Currently, we focus on Skype and have not tested it on the other channels.

## External services

The bot service uses the following external services:

- [LUIS](https://www.luis.ai/) for natural language processing
- [Azure Blob Storage](https://azure.microsoft.com/en-us/documentation/articles/storage-introduction/) for BMA model file storage
- [Bing Spell Check API](https://www.microsoft.com/cognitive-services/en-us/bing-spell-check-api) for cases when LUIS did not understand the input
- [BMA Backend](http://bmamath.cloudapp.net/api/) for testing formulae within a conversation

LUIS and Azure Blob Storage are hard dependencies and the bot will not work if any of those are not available.

Bing Spell Check API and BMA Backend are soft dependencies which, if not available, will lead to a graceful degradation of the bot
(like not handling spelling mistakes, or not testing formulae directly) but otherwise do not affect the bot's functionality.

## Storage

### Conversation state

When deployed, then all conversation data is stored by the Bot Framework automatically.

Quoting from the [Technical FAQ](https://docs.botframework.com/en-us/technical-faq/#where-is-conversation-state-stored):

> The Bot Framework Connector provides a cloud service that implements this interface and stores data in Azure.
  This data is encrypted at rest and does not intentionally expire.

Conversation storage is associated to the Microsoft APP ID and password of the bot.

Currently, stored data is not explicitly removed in any way.
All data is stored per-conversation, not per-user.
Note that for regular Skype (not "for Business") an old conversation is automatically reused,
except if the user explicitly clears the conversation.
Therefore, in most cases, storage does not "explode", but this should be re-evaluated.

The following information is currently stored in the conversation state (`session.conversationData`):

- Formula history (`.formulas`)
- Uploaded BMA model (`.bmaModel`)
- Azure Blob Storage key (randomly generated uuid) for uploaded BMA model (`.bmaModelId`) 
- Whether spell checking was already performed (`.hasSpellChecked`)
- Last message text when the bot queries for the user's BMA model, in order to process it again after model uploading (`.lastMessageText`)

### Uploaded/generated BMA models

When the user sends the bot a BMA model file, or when the bot generates such file,
then it is stored in Azure Blob Storage. This is done so that a URL can be generated for the model
which in turn can be used as `?Model=https://...` parameter for the BMA web tool.
The URLs currently do not have any access or other restrictions and are fully public for anyone knowing the URL.
Since the URL includes a UUID of the form `109156be-c4fb-41ea-b1b4-efe1671c5836` it is hard to guess the URL just by random trying.

Note that currently, stored BMA models do not expire and are never removed.
This should be implemented when going into production.

## Dialogs

When a user starts to chat with the bot, the message is first processed by the [trained LUIS model](luis.md)
to understand the *intent* of the message. Based on the intent, either a simple response is returned,
or a more complex dialog is started, e.g. when asking for [tutorials](tutorials.md).
Complex dialogs are kept to a minimum to reduce complexity in implementation.
See also the "Dialogs" slide of the [final traineeship presentation][trainee-final-ppt].

A special case is when the [LUIS model](luis.md) recognizes the message as `LTLQuery` intent.
An example is "show me a simulation where at some point CheA is less than 3".
In that case, a [custom LTL-NL parser](NLParser.md) is invoked which tries to parse the formula.
The parsing result is an abstract syntax tree which gets further transformed into responses containing:

- a human-readable canonical string that can be copy-pasted into the BMA web tool,
- a link to the BMA web tool pointing to an ad-hoc generated BMA model file containing the given formula,
- the result of invoking the BMA Backend on the formula.

When invoking the BMA Backend there currently is a timeout of 3+3 seconds (two API queries are necessary).
This means that in some cases the bot responds that there was not enough time to test the formula and
the user should instead click on the link to the generated BMA model to see the results.

If no intent was recognized then one of two things happen:

- If the user sent an attachment, handle it as the new user BMA model. (In that case, the message text was empty.)
- If there is no attachment, do a Bing Spell Check on the message text and try to recognize it again once.
