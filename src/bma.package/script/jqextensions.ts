// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
declare module JQueryUI {
    interface DraggableOptions {
        start?: DraggableEvent;
        stop?: DraggableEvent;
        drag?: DraggableEvent;
    }

    interface DroppableOptions {
        drop?: DroppableEvent;
    }

    interface TooltipOptions {
        close?: TooltipEvent;    
    }
}
