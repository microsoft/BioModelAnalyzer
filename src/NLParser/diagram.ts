// Copyright (C) 2016 Microsoft - All Rights Reserved

// run this using `npm run grammarViz` which will compile it and open a browser automatically

import * as diagrams from 'chevrotain/diagrams/src/main'
import NLParser from './NLParser'

var parserInstanceToDraw = new NLParser([])
var diagramsDiv = document.getElementById('diagrams')
diagrams.drawDiagramsFromParserInstance(parserInstanceToDraw, diagramsDiv)
