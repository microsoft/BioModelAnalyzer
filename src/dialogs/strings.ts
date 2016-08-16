export let LTL_DESCRIPTION = 'LTL means linear temporal logic'
export let UNKNOWN_INTENT = 'I did not understand you'
export let MODEL_SEND_PROMPT = 'Please send me your model as a JSON file'
export let INVALID_JSON = (msg: string) => `Your uploaded file is not valid JSON (Error: ${msg})`
export let HTTP_ERROR = (msg: string) => `HTTP Error: ${msg}`
export let TOO_MANY_FILES = 'Please upload exactly one JSON file'
export let MODEL_RECEIVED = (name: string) => `I received your model titled ${name} and will use it from now on`
export let TUTORIAL_SELECT_PROMPT = 'Which tutorial would you like to do?'
export let TUTORIAL_START_PROMPT = 'Would you like to start the tutorial?'
export let TUTORIAL_SELECT_CANCELLED = 'Tutorial selection cancelled.'
export let TUTORIAL_UNKNOWN_SELECT = 'Please input a tutorial number, or anything else to cancel.'
export let ABOUT_BOT = 'Hello, I am the BMA Bot and I will assist you in creating LTL formulas to query your model. To help you along the way, I will provide you with tutorials and examples. Ask me any questions you have about using the BMA and I will try to help you.'
export let ABOUT_SIMULATIONS = 'LTL queries are used to find simulations of a given length that end in a loop (a fix point or cycle)'
export let SPELLCHECK_ASSUMPTION = (corrected: string) => `I assume you meant: "${corrected}"`
export let OPEN_BMA_URL = (url: string) => `Open in browser: ${url}`
export let HERE_IS_YOUR_UPLOADED_MODEL = (url: string) => `Here is the model you sent me: ${url}`

//Explaination for each operator 

export let EXPLAIN_AND = 'The AND operator returns a true value if both the expressions are true, and returns false otherwise'
export let EXPLAIN_OR = 'The OR operator returns a true value if either one of the expressions are true, otherwise false is returned'
export let EXPLAIN_IMPLIES = 'When two expressions are used with the implies operator, e.g. A implies B, it means that if A is true B must also be true. The IMPLIES operator returns a true value in this scenario.'
export let EXPLAIN_NOT = 'The NOT operator returns a true value if the expression is false, and returns false if it equals to true'
export let EXPLAIN_NEXT = 'Within a state model, The NEXT operator returns true if the immediate state after holds a true value'
export let EXPLAIN_ALWAYS = 'Within a state model, the always operator returns true if all the states hold a true value'
export let EXPLAIN_EVENTUALLY = 'Within a state model, the EVENTUALLY operator returns true if some state in the future holds a true value'
export let EXPLAIN_UNTIL = 'Similar to the AND operator, the UNTIL operator requires two operands. For UNTIL, A until B, implies that A remains true until B becomes true'
export let EXPLAIN_RELEASE = 'This operator requires two operands. In the scenario, A release B: B holds a true value until and including the point when A first becomes true. If A is never true, B will remain true'
//TODO explain UPTO
export let EXPLAIN_UPTO = 'This explains the upto operator '
export let EXPLAIN_WEAKUNTIL = 'Similar to the UNTIL operator, in the example A weakuntil B, A remains true until B holds a true value, however it does not require B to ever hold a true value, and therefore A always remains true' 