/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ProfileMenuComponent implements OnInit, OnDestroy {
    private authenticationSubscription: Subscription;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profilePictureUrl = '';

    public isAdmin = false;

    public profileUrl = this.apiUrl.buildUrl('/identity-server/account/profile');

    constructor(
        private readonly auth: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.auth.isAuthenticated.take(1)
                .subscribe(() => {
                    const user = this.auth.user;

                    if (user) {
                        this.profilePictureUrl = user.pictureUrl;
                        this.profileDisplayName = user.displayName;

                        this.isAdmin = user.isAdmin;
                    }
                });
    }

    public logout() {
        this.auth.logoutRedirect();
    }
}