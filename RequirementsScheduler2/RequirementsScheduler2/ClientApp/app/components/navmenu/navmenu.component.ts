import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthenticationService } from '../../services/authentication.service';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'nav-menu',
    template: require('./navmenu.component.html'),
    styles: [require('./navmenu.component.css')]
})
export class NavMenuComponent implements OnInit, OnDestroy {
    isLogged : boolean;

    subscription: Subscription;

    constructor(private _authenticationService: AuthenticationService) { }

    ngOnInit() {
        this.subscription = this._authenticationService.isLogged
            .subscribe(item => this.isLogged = item);
    }

    ngOnDestroy() {
        // prevent memory leak when component is destroyed
        this.subscription.unsubscribe();
    }
}
