import { Component } from '@angular/core';
import { User } from "../../models/user";
import { UserService } from "../../services/user.service";

@Component({
    selector: 'home',
    template: require('./home.component.html')
})

export class HomeComponent {
    model: any = {};
    currentUser: User;

    constructor(private userService: UserService) {
        this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
    }
}
