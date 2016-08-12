# Specification of tutorial file format

```yaml
id: ltl_for_dummies
title: LTL for dummies
description: |-
  This tutorial guides you through...
  It takes around x min.
  When you're done, you will know how to...
steps:
  # after each step of the dialog, the bot waits for the user to say "next" or similar
  - text: |-
      This is the first step.
      The above |- indicator is for keeping newlines intact.
      The minus omits the newline at the end of the whole text.
    timeout: 

  - text: |-
      Another message to the user.

    # paths are relative to /public/tutorials/img/
    image: ltl_for_dummies_1.jpg
  
  - type: 
    

```