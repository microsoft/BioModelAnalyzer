# Tutorials

All tutorials are stored in the /data/tutorials folder as YAML files.

A user can access the list of tutorials with messages such as:

- "show me a list of tutorials"
- "which tutorials can I do"
- "which tutorials do you have?"
- "tutorials"

## How to add a new tutorial

1. Create a new [YAML](https://en.wikipedia.org/wiki/YAML) file in the /data/tutorials folder by copying and pasting an existing one.
2. Change the texts, model and image references in the YAML file (see "YAML format" section below).
3. Add the tutorial to the `TUTORIALS` array in the /src/dialogs/tutorials.ts file.
4. Deploy the bot.

## YAML format

A tutorial is a simple YAML file which has some metadata plus the actual tutorials steps.
Each tutorial step can contain one or more of the following, typically at least text:

- Text
- Image, sent to the user as an attachment
- BMA model, sent to the user as a parameterized link to the BMA web tool

If a given step is explained with an image or a model, then the `image:` and/or `model:` property
should be in the located in the same step instead of as a separate step.
The reason for that is that the steps get numbered, e.g. `[2/13]`, and the image/model would appear
under the same step numbering. This makes it easier for users to know where an image/model belongs to.

Different to JSON, YAML allows to have multiline strings and even comments which makes it very suitable
for this purpose.

The following is an example of a tutorial YAML file: 

```yaml
id: ltl_for_dummies
title: LTL for dummies
description: >-
  This tutorial guides you through...
  It takes around x min.
  When you're done, you will know how to...
steps:
  - text: >-
      This is the first step.
      The above >- indicator is for keeping paragraphs intact (separated by an empty newline).

      The minus omits the newline at the end of the whole text.

  - text: >-
      Another message to the user.

    # send an image to the user
    # image paths are relative to /public/tutorials/img/
    image: ltl_for_dummies_1.jpg

    # send a link to the user which opens the given model in BMA    
    # model paths are relative to /public/tutorials/model/
    model: ecoli.json
```
