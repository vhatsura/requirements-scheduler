import { Component, OnInit } from '@angular/core';
import { User, Experiment } from "../../models/index";
import { ExperimentService, AlertService } from "../../services/index";
import { isBrowser } from 'angular2-universal';

@Component({
    selector: 'home',
    template: require('./home.component.html')
})

export class HomeComponent implements OnInit {
    currentUser: User;

    ngOnInit(): void {
        if (isBrowser) {
            this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
        }
    }
}
