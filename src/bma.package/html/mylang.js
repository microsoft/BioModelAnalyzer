// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
var targetfunc = {
  // Set defaultToken to invalid to see what you do not tokenize yet
  //defaultToken: 'invalid',

  keywords: [
    'min', 'max', 'avg', 'ceil', 'floor'
  ],

  operators: [ '+', '-', '*', '/'
  ],

  brackets: [
      ['(', ')','delimiter.parenthesis'] 
  ],

  // we include these common regular expressions
  symbols:  /[=><!~?:&|+\-*\/\^%]+/,

  numbers: /(?:[+-])?(?:(?:(?:\d*\.)?\d+)(?:[eE][-+]?\d+)?)/,

  // The main tokenizer for our languages
  tokenizer: {
    root: [
      // identifiers and keywords
      [/(var\s*)(\()([^)]+)(\))/, ['keyword','@brackets','identifier','@brackets']],

      [/(const\s*)(\()(\s*@numbers\s*)(\))/, ['keyword','@brackets', 'number','@brackets']],

      [/[A-Za-z_$][\w$]*/, { cases: { 
                                   '@keywords': 'keyword',
                                   '@default': 'invalid' } }
      ],

      // whitespace
      { include: '@whitespace' },

      // delimiters and operators
      [/[()]/, '@brackets'],
      [/@symbols/, { cases: { '@operators': 'operator',
                              '@default'  : '' } } ],

      // numbers
      [/\d*\.\d+([eE][\-+]?\d+)?/, 'number.float'],
      [/\d+/, 'number'],

      // delimiter: after number because of .\d floats
      [/[,]/, 'delimiter'],
    ],

    whitespace: [
      [/[ \t\r\n]+/, 'white']
    ]
  }
};
