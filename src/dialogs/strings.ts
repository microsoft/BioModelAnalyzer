export let LTL_DESCRIPTION = 'LTL means linear temporal logic'
export let UNKNOWN_INTENT = 'I did not understand you'
export let MODEL_SEND_PROMPT = 'Please send me your model as a JSON file'
export let INVALID_JSON = (msg: string) => `Your uploaded file is not valid JSON (Error: ${msg})`
export let HTTP_ERROR = (msg: string) => `HTTP Error: ${msg}`
export let TOO_MANY_FILES = 'Please upload exactly one JSON file'
export let MODEL_RECEIVED = (name: string) => `I received your model titled ${name} and will use it from now on`
export let TUTORIAL_SELECT_PROMPT = 'Which tutorial would you like to do?'
export let TUTORIAL_START_PROMPT = 'Do you like to start the tutorial?'
export let TUTORIAL_SELECT_CANCELLED = 'Tutorial selection cancelled.'
export let TUTORIAL_UNKNOWN_SELECT = 'Please input a tutorial number, or anything else to cancel.'
export let ABOUT_BOT = 'Hello, I am the BMA Bot and I will assist you in creating LTL formulas to query your model. To help you along the way, I will provide you with tutorials and examples.'
export let SPELLCHECK_ASSUMPTION = (corrected: string) => `I assume you meant: "${corrected}"`

//TODO explain each operator below and confirm with BMA users that the definitions are sufficient

export let EXPLAIN_AND = 'this explains the and operator'
export let EXPLAIN_OR = 'this explains the OR operator'
export let EXPLAIN_IMPLIES = 'this explains the IMPLIES operator'
export let EXPLAIN_NOT = 'this explains the NOT operator'
export let EXPLAIN_NEXT = 'this explains the NEXT operator'
export let EXPLAIN_ALWAYS = 'this explains the always operator'
export let EXPLAIN_EVENTUALLY = 'this explains the EVENTUALLY operator'
export let EXPLAIN_UPTO = 'this explains the UPTO operator'
export let EXPLAIN_WEAKUNTIL = 'this explains the WEAKUNTIL operator'
export let EXPLAIN_UNTIL = 'this explains the UNTIL operator'
export let EXPLAIN_RELEASE = 'this explains the RELEASE operator'