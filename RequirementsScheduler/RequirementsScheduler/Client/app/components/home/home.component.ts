import { Component, Inject, OnInit } from '@angular/core';
import { User, Experiment } from '../../models/index';
import { ExperimentService, AlertService } from '../../services/index';

import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})

export class HomeComponent implements OnInit {
    currentUser: User;

    constructor( @Inject(PLATFORM_ID) private platformId: Object) { }

    isBrowser() {
        return isPlatformBrowser(this.platformId);
    }

    ngOnInit(): void {
        if (this.isBrowser()) {
            this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
        }
    }
}
