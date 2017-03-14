import { Component, OnInit } from '@angular/core';
import { GtExpandedRow } from 'angular2-generic-table';

@Component({
    selector: 'experiment-detail',
    template: require('./experiment-detail.component.html')
})
export class ExperimentDetailComponent extends GtExpandedRow<any> implements OnInit {

    constructor() { super() }

    ngOnInit() {
    }
}