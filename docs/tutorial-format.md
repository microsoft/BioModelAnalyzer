# Specification of tutorial file format

See /data/tutorials for all available tutorials.

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