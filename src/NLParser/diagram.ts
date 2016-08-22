// run this using `npm run diagram` which will compile it and open a browser automatically

// see the diagrams/README subfolder
//import * as diagrams from 'chevrotain/diagrams/src/main'

import * as diagrams from './diagrams/main'
import NLParser from './NLParser'

var parserInstanceToDraw = new NLParser([])
var diagramsDiv = document.getElementById("diagrams")
diagrams.drawDiagramsFromParserInstance(parserInstanceToDraw, diagramsDiv)