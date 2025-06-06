import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutComponent } from '../../../../shared/layout/layout.component';
import { LivraisonDialogComponent } from '../livraison-dialog/livraison-dialog.component';
import { Livraison } from '../../../../core/models/livraison.model';
import { LivraisonService } from '../../services/livraison.service';
import { PermissionService } from '../../../../core/services/PermissionService';
import { AuthService } from '../../../../core/auth/auth.service';
import { AuditService } from '../../../../core/services/audit.service';


@Component({
  selector: 'app-livraison-list',
  standalone: true,
  imports: [CommonModule, RouterModule, LayoutComponent, LivraisonDialogComponent],
  templateUrl: './livraison-list.component.html',
  styleUrls: ['./livraison-list.component.css']
})
export class LivraisonListComponent implements OnInit {
  @ViewChild(LivraisonDialogComponent) livraisonDialog!: LivraisonDialogComponent;
  
  livraisons: Livraison[] = [];
  loading = false;
  error: string | null = null;
  
  constructor(
    private livraisonService: LivraisonService,
    private permissionService: PermissionService,
    private authService: AuthService,
    private auditService: AuditService
  ) { }
  
  // Permission check methods
  canEditLivraisons(): boolean {
    return this.permissionService.hasPermission('Livraisons.Edit');
  }
  canDeleteLivraisons(): boolean {
    return this.permissionService.hasPermission('Livraisons.Delete');
  }
  canCreateLivraisons(): boolean {
    return this.permissionService.hasPermission('Livraisons.Create');
  }
  ngOnInit(): void {
    this.loadLivraisons();
  }
  
  loadLivraisons(): void {
    this.loading = true;
    this.livraisonService.getLivraisons().subscribe({
      next: (livraisons) => {
        this.livraisons = livraisons.sort((b, a) => a.numero.localeCompare(b.numero));
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load livraisons';
        this.loading = false;
        console.error(err);
      }
    });
  }

  openAddLivraisonDialog(): void {
    this.livraisonDialog.open();
  }
  
  deleteLivraison(id: string): void {
    if (confirm('Are you sure you want to delete this livraison?')) {
      // Find the livraison before deleting to get its number
      const livraisonToDelete = this.livraisons.find(livraison => livraison.id === id);
      if (!livraisonToDelete) {
        console.error('Livraison not found');
        return;
      }
      
      const numeroLivraison = livraisonToDelete.numero;
      
      this.livraisonService.deleteLivraison(id).subscribe({
        next: () => {
          this.livraisons = this.livraisons.filter(livraison => livraison.id !== id);
          
          // Post the audit log after successful deletion
          const userId = this.authService.getCurrentUserId();
          if (userId) {
            this.auditService.postAudit(userId, 'Suppression', numeroLivraison).subscribe({
              next: () => {
                console.log('Audit posted successfully');
              },
              error: (err) => {
                if (err.status === 201) {
                  console.log('Audit posted successfully with 201 status');
                } else {
                  console.error('Failed to post audit:', err);
                }                }
            });
          }
        },
        error: (err) => {
          console.error('Failed to delete livraison:', err);
          this.error = 'Failed to delete livraison.';
        }
      });
    }
  }
}