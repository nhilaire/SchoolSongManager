# Organisation des styles - Composant Library

Ce document décrit l'organisation modulaire des styles du composant Library.

## Structure des fichiers

### `_variables.scss`
**Rôle** : Définition centralisée de toutes les variables Sass
- **Couleurs** : palette de couleurs cohérente (primaire, secondaire, texte, arrière-plans)
- **Espacements** : système d'espacement standardisé (xs, sm, md, lg, xl, etc.)
- **Rayons de bordure** : tailles de border-radius réutilisables
- **Ombres** : effets d'ombre standardisés
- **Transitions** : durées et types de transitions

### `_layout.scss`
**Rôle** : Structure générale et mise en page
- Container principal
- En-tête avec titre et bouton d'ajout
- États de chargement et d'erreur
- État vide (pas de comptines)

### `_buttons.scss`
**Rôle** : Tous les styles de boutons
- Bouton d'ajout principal
- Boutons d'édition et suppression
- Boutons de modal (fermer, annuler, sauvegarder)
- États hover et disabled

### `_rhyme-cards.scss`
**Rôle** : Styles des cartes de comptines
- Grille responsive des cartes
- Structure interne des cartes
- En-tête avec titre et actions
- Contenu (paroles, audio)
- Pied de page avec date

### `_modal.scss`
**Rôle** : Styles du système de modal
- Overlay de fond
- Conteneur de modal
- En-tête de modal
- Actions de modal

### `_forms.scss`
**Rôle** : Styles des formulaires
- Structure des groupes de champs
- Labels et inputs
- Textarea
- Messages d'aide
- États focus et validation

### `library.component.scss`
**Rôle** : Point d'entrée principal
- Importe tous les fichiers partiels avec `@use`
- Utilise la syntaxe moderne Sass

## Avantages de cette organisation

### 🎯 **Maintenabilité**
- Chaque fichier a une responsabilité claire
- Modifications isolées par domaine fonctionnel
- Facilite le debugging des styles

### 🔄 **Réutilisabilité**
- Variables centralisées réutilisables
- Composants de styles modulaires
- Facilite la création de nouveaux composants

### 👥 **Collaboration**
- Structure claire pour les développeurs
- Évite les conflits de merge
- Standards de codage cohérents

### 📦 **Performance**
- Compilation optimisée
- Pas de duplication de code CSS
- Bundles finaux identiques

## Conventions de nommage

### Variables
- `$primary-color`, `$secondary-color` : couleurs principales
- `$spacing-xs`, `$spacing-sm` : système d'espacement
- `$border-radius-sm`, `$border-radius-md` : rayons de bordure
- `$transition-default` : transition standard

### Classes
- `.component-container` : conteneur principal
- `.component-header` : en-tête de section
- `.component-actions` : zone d'actions
- Préfixes cohérents par section

## Migration future

Cette structure facilite :
- **Thèmes** : modification des variables pour changer l'apparence
- **Responsive** : ajout de breakpoints centralisés
- **Dark mode** : surcharge de variables de couleurs
- **Design system** : extraction vers une bibliothèque partagée